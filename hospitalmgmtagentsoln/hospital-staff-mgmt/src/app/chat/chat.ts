import { Component, ElementRef, NgZone, ViewChild,ChangeDetectorRef, OnInit } from '@angular/core';
import { AgentService } from '../services/agent.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FullCalendarComponent } from '@fullcalendar/angular';
import { SmartSuggestion } from '../models/smart-suggestion.model';
import { SmartSuggestionsService } from '../services/smart-suggestion.service';


@Component({
  selector: 'app-chat',
  templateUrl: './chat.html',
  standalone: true,
  imports: [CommonModule,FormsModule ],
  styleUrls: ['./chat.css']
})
export class Chat implements OnInit {
  messageText: string = '';
  messages: { sender: string, text: string }[] = [];
  isWaiting = false;
  showChat = true;
  suggestions: SmartSuggestion[] = [];
//   suggestions: string[] = [
//   "Show my shifts for this week",
//   "Request leave for tomorrow",
//   "Swap shift with Anjali",
//   "Who's working in ICU today?",
//   "Cancel my evening shift on Friday"
// ];



  
  @ViewChild('chatContainer') chatContainer!: ElementRef;

  constructor(private agentService: AgentService, private smartSuggestionService: SmartSuggestionsService,
    private ngZone: NgZone,private cdRef: ChangeDetectorRef) {}

  ngOnInit(): void {
  this.loadSmartSuggestions();
  this.messages.push({
    sender: 'Agent',
    text: '👋 Hello! I am your hospital assistant. How can I help you today?'
  });
}



 loadSmartSuggestions(): void {
    this.smartSuggestionService.getSmartSuggestions().subscribe({
      next: (data:SmartSuggestion[]) => (this.suggestions = data),
      error: (err:any) => console.error('Failed to load smart suggestions', err)
    });
  }

  removeSuggestion(index: number): void {
  this.suggestions.splice(index, 1);
}

applySuggestion(suggestion: string) {
  this.messageText = suggestion;
  this.sendMessage();  // Optionally send immediately
}

  handleSuggestionClick(s: SmartSuggestion): void {
    this.sendMessage(s.actionPayload); // Treat as if user typed it
  }

  sendMessage(action?: string): void {
    const messageToSend = action?.trim() || this.messageText.trim();
    if (!messageToSend || this.isWaiting) return; // Prevent empty or double submit

    // Push user's message
    this.messages.push({ sender: 'User', text: messageToSend });
    this.scrollToBottom();

    // Reset input only if typed
    if (!action) {
      this.messageText = '';
    }

    this.isWaiting = true;

    // Call agent service with the actual message (either typed or from chip)
    this.agentService.askAgent(messageToSend).subscribe({
      next: (response) => {
        this.messages.push({ sender: 'Agent', text: response.reply || '🤖 (No reply)' });
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
