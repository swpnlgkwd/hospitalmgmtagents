import { Component, ElementRef, NgZone, ViewChild,ChangeDetectorRef, OnInit } from '@angular/core';
import { AgentService } from '../services/agent.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-chat',
  templateUrl: './chat.html',
  standalone: true,
  imports: [CommonModule,FormsModule ],
  styleUrls: ['./chat.css']
})
export class Chat   implements OnInit {
  messageText: string = '';
  messages: { sender: string, text: string }[] = [];
  isWaiting = false;
  showChat = true;

  @ViewChild('chatContainer') chatContainer!: ElementRef;

  constructor(private agentService: AgentService, private ngZone: NgZone,private cdRef: ChangeDetectorRef) {}

  ngOnInit(): void {
  this.messages.push({
    sender: 'Agent',
    text: '👋 Hello! I am your hospital assistant. How can I help you today?'
  });
}

sendMessage(): void {
  const question = this.messageText.trim();
  if (!question) return;

  this.messages.push({ sender: 'User', text: question });
  this.scrollToBottom(); // 👈 ADD HERE
  this.messageText = '';
  this.isWaiting = true;

  this.agentService.askAgent(question).subscribe({
    next: (response) => {
      this.messages.push({ sender: 'Agent', text: response.reply });
      this.isWaiting = false;
      this.cdRef.detectChanges(); // 👈 force UI to update
      this.scrollToBottom();
    },
    error: (error) => {
      this.messages.push({
        sender: 'Agent',
        text: '⚠️ Something went wrong. Please try again later.',
      });
      this.isWaiting = false;
      this.cdRef.detectChanges();
      this.scrollToBottom();
    }
  });
}


  scrollToBottom(): void {
    setTimeout(() => {
      try {
        this.chatContainer.nativeElement.scrollTop = this.chatContainer.nativeElement.scrollHeight;
      } catch (e) {
        console.error('Scroll error:', e);
      }
    }, 100);
  }

  toggleChat() {
    this.showChat = !this.showChat;
  }
}
