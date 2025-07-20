import { Component } from '@angular/core';
import { Chat } from '../chat/chat';
import { ShiftCalendarComponent } from '../shift-calendar.component/shift-calendar.component';

@Component({
  selector: 'app-main-layout',
  imports: [Chat,ShiftCalendarComponent],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css'
})
export class MainLayout {

}
