import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SmartSuggestion } from '../models/smart-suggestion.model';
import { AgentSummaryResponse } from '../models/agent-daily-summary.model';

@Injectable({
  providedIn: 'root'
})
export class SmartSuggestionsService {
  private baseUrl = 'http://localhost:5095/api/SmartSuggestions/suggestions'; // Update with actual base URL

  constructor(private http: HttpClient) { }

  getSmartSuggestions(): Observable<SmartSuggestion[]> {
    return this.http.get<SmartSuggestion[]>(this.baseUrl);
  }
  getAgentInsights(): Observable<{ message: string }> {
    return this.http.get<{ message: string }>('http://localhost:5095/api/SmartSuggestions/agentinsights');
  }
  getDailySummary(): Observable<AgentSummaryResponse> {
    return this.http.get<AgentSummaryResponse>('http://localhost:5095/api/SmartSuggestions/agent-scheduler');
  }
}

