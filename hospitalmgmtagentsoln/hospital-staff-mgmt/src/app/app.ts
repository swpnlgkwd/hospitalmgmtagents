import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router'; 
import { Chat } from './chat/chat';
import { ShiftCalendarComponent } from './shift-calendar.component/shift-calendar.component';
 

@Component({
  selector: 'app-root',
  standalone: true, 
  imports:[Chat,ShiftCalendarComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('hospital-staff-mgmt');
}
