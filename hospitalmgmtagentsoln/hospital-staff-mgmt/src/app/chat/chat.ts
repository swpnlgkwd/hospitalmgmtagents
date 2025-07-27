import { Component, ElementRef, NgZone, ViewChild, ChangeDetectorRef, OnInit } from '@angular/core';
import { AgentService } from '../services/agent.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FullCalendarComponent } from '@fullcalendar/angular';
import { SmartSuggestion } from '../models/smart-suggestion.model';
import { SmartSuggestionsService } from '../services/smart-suggestion.service';
import { QuickReply } from '../models/agent-daily-summary.model';


@Component({
  selector: 'app-chat',
  templateUrl: './chat.html',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styleUrls: ['./chat.css']
})
export class Chat implements OnInit {
  messageText: string = '';

  messages: { sender: string, text: string, quickReplies?: QuickReply[]; }[] = [];
  isWaiting = false;
  showChat = false;
  suggestions: SmartSuggestion[] = [];





  @ViewChild('chatContainer') chatContainer!: ElementRef;

  constructor(private agentService: AgentService, private smartSuggestionService: SmartSuggestionsService,
    private ngZone: NgZone, private cdRef: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.loadSmartSuggestions();

  }

  handleActionClick(s: SmartSuggestion): void {
    // const actionMessage = {
    //   type: s.type,
    //   actionName: s.actionName,
    //   data: s.actionData || {}
    // };

    // const messagePayload = `ACTION::${JSON.stringify(actionMessage)}`;

    this.sendMessage(s.actionText); // This should route to your chat API
  }

  loadSmartSuggestions(): void {
    // Load proactive action suggestions
    this.smartSuggestionService.getSmartSuggestions().subscribe({
      next: (data: SmartSuggestion[]) => {
        this.suggestions = data;
      },
      error: (err: any) => {
        console.error('Failed to load smart suggestions', err);
      }
    });

     // Show typing animation first
    this.isWaiting = true;
    
    // On init or scheduler landing
 // Get daily summary with a delay to simulate typing
  this.smartSuggestionService.getDailySummary().subscribe({
    next: (response) => {
      const message = response?.summaryMessage?.trim();

      // Simulate typing delay (2 seconds)
      setTimeout(() => {
        if (message) {
          this.messages.push({
            sender: 'Agent',
            text: message,
            quickReplies: response.quickReplies
          });
        } else {
          this.messages.push({
            sender: 'Agent',
            text: '👋 Hello! I am your hospital assistant. How can I help you today?'
          });
        }
        this.isWaiting = false;
        this.cdRef.detectChanges();
      }, 4000); // 2-second delay
    },
    error: (err: any) => {
      console.error('Failed to load daily agent summary', err);

      // Show fallback message after delay
      setTimeout(() => {
        this.messages.push({
          sender: 'Agent',
          text: '👋 Hello! I am your hospital assistant. How can I help you today?'
        });
        this.isWaiting = false;
        this.cdRef.detectChanges();
      }, 2000); // Delay fallback too
    }
  });
}
 

  

  handleQuickReply(reply: { label: string; value: string }) {
    
    for (let i = this.messages.length - 1; i >= 0; i--) {
      const message = this.messages[i];
      if (message.sender === 'Agent' && message.quickReplies?.length) {
        delete message.quickReplies;
        break;
      }
    }

     this.cdRef.detectChanges();

    // Process the reply (you can call your service or tool here)
   this.sendMessage(reply.value);  // assuming you have this method
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
