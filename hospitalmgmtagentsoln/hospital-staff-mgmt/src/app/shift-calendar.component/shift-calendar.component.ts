import { Component, OnInit, ViewChild } from '@angular/core';
import { FullCalendarComponent, FullCalendarModule } from '@fullcalendar/angular'; // standalone module
import { CalendarOptions } from '@fullcalendar/core/index.js';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';
import timeGridPlugin from '@fullcalendar/timegrid';
import { ShiftScheduleService } from '../services/shift-schedule.service';

@Component({
  selector: 'app-shift-calendar',
  standalone: true,
  imports: [FullCalendarModule],
  templateUrl: './shift-calendar.component.html',
  styleUrl: './shift-calendar.component.css'
})
export class ShiftCalendarComponent implements OnInit {

  constructor(private scheduleService: ShiftScheduleService) { }

  calendarOptions: CalendarOptions = {
    plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
    initialView: 'dayGridMonth',
    aspectRatio: 1.4, // Optional: makes cells taller
    headerToolbar: {
      left: 'prev,next today',
      center: 'title',
      right: 'dayGridMonth,timeGridWeek,listWeek'
    },
    events: [], // Initially empty
    dateClick: this.handleDateClick.bind(this),
    eventClick: this.handleEventClick.bind(this),

    // 👉 Add this here
    eventDidMount: (info) => {
      const { staffName, departmentName, shiftType } = info.event.extendedProps;

      const content = `
      <div class="shift-title">
        <div class="staff-name">${staffName}</div>
        <div class="details">(${departmentName} - ${shiftType})</div>
      </div>
    `;

      const titleEl = info.el.querySelector('.fc-event-title');
      if (titleEl) {
        titleEl.innerHTML = content;
      }
    }
  };



  ngOnInit(): void {
    const startDate = '2025-07-01';
    const endDate = '2025-07-31';

    this.scheduleService.fetchShiftInformation().subscribe({
      next: shifts => {
        const events = this.transformShiftsToEvents(shifts);
        console.log('Fetched shifts:', events);

        // Important: Create a new object to trigger change detection
        this.calendarOptions = {
          ...this.calendarOptions,
          events: events
        };
        console.log('Updated calendar options:', this.calendarOptions);
      },
      error: err => {
        console.error('Failed to fetch shifts:', err);
      }
    });
  }

  handleDateClick(arg: any): void {
    alert('Date clicked: ' + arg.dateStr);
  }

  handleEventClick(arg: any): void {
    alert('Shift: ' + arg.event.title);
  }

transformShiftsToEvents(shifts: any[]): any[] {
  return shifts.map(shift => {
    const isVacant = shift.staffName === 'Vacant';

    return {
      title: isVacant
        ? `🟡 Vacant (${shift.shiftType})`
        : `${shift.staffName} (${shift.shiftType})`,
      start: shift.shiftDate,
      end: shift.shiftDate, // Optional: calculate based on shift type duration
      allDay: true,
      extendedProps: {
        staffName: shift.staffName,
        departmentName: shift.departmentName,
        shiftType: shift.shiftType,
        role: shift.role,
        isVacant: isVacant
      },
      backgroundColor: isVacant
        ? '#fff3cd' // light yellow
        : this.getShiftColor(shift.shiftType),
      borderColor: isVacant
        ? '#ffc107' // yellow border
        : this.getBorderColor(shift.shiftType),
      textColor: isVacant ? '#856404' : undefined
    };
  });
}


  getShiftColor(shiftType: string): string {
    switch (shiftType.toLowerCase()) {
      case 'morning': return '#4caf50';
      case 'evening': return '#ff9800';
      case 'night': return '#2196f3';
      default: return '#9e9e9e';
    }
  }

  getBorderColor(shiftType: string): string {
    switch (shiftType.toLowerCase()) {
      case 'morning': return '#388e3c';
      case 'evening': return '#f57c00';
      case 'night': return '#1976d2';
      default: return '#616161';
    }
  }
}
