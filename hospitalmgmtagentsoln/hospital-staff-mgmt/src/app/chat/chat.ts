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
  suggestions: string[] = [
  "Show my shifts for this week",
  "Request leave for tomorrow",
  "Swap shift with Anjali",
  "Who's working in ICU today?",
  "Cancel my evening shift on Friday"
];

  @ViewChild('chatContainer') chatContainer!: ElementRef;

  constructor(private agentService: AgentService, private ngZone: NgZone,private cdRef: ChangeDetectorRef) {}

  ngOnInit(): void {
  this.messages.push({
    sender: 'Agent',
    text: '👋 Hello! I am your hospital assistant. How can I help you today?'
  });
}

applySuggestion(suggestion: string) {
  this.messageText = suggestion;
  this.sendMessage();  // Optionally send immediately
}

sendMessage(): void {
  const question = this.messageText.trim();
  if (!question || this.isWaiting) return; // prevent double submission

  // Push user's message
  this.messages.push({ sender: 'User', text: question });
  this.scrollToBottom();

  // Reset input and set loading
  this.messageText = '';
  this.isWaiting = true;

  // Call agent service
  this.agentService.askAgent(question).subscribe({
    next: (response) => {
      this.messages.push({ sender: 'Agent', text:  response.reply || '🤖 (No reply)' });
      this.isWaiting = false;
      this.cdRef.detectChanges();
      this.scrollToBottom();
    },
    error: (error) => {
      console.error('Agent error:', error);
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
