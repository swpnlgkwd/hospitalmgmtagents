import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ChatResponse } from '../models/chat-response.model';

@Injectable({ providedIn: 'root' })
export class AgentService {
  constructor(private http: HttpClient) {}

  askAgent(userMessage: string): Observable<ChatResponse> {
    const body = { message: userMessage }; // Matches the C# model
     return this.http.post<ChatResponse>('http://localhost:5095/AgentChat/ask', body );

  }
}