import { Component, signal } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router'; 
import { Chat } from './chat/chat';
import { ShiftCalendarComponent } from './shift-calendar.component/shift-calendar.component';
import { Header } from './header/header';
import { CommonModule } from '@angular/common';


@Component({
  selector: 'app-root',
  standalone: true, 
  imports:[Header, RouterModule,CommonModule],
  
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('hospital-staff-mgmt');
  isLoggedIn(): boolean {
    return !!localStorage.getItem('token'); // or your token key
  }

}
