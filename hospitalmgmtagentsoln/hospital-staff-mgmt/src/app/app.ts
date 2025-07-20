import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router'; 
import { Chat } from './chat/chat';
 

@Component({
  selector: 'app-root',
  standalone: true, 
  imports:[Chat]  ,
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('hospital-staff-mgmt');
}
