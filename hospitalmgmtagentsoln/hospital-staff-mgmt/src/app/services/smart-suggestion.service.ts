import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SmartSuggestion } from '../models/smart-suggestion.model';

@Injectable({
  providedIn: 'root'
})
export class SmartSuggestionsService {
  private baseUrl = 'http://localhost:5095/api/SmartSuggestions'; // Update with actual base URL

  constructor(private http: HttpClient) {}

  getSmartSuggestions(): Observable<SmartSuggestion[]> {
    return this.http.get<SmartSuggestion[]>(this.baseUrl);
  }
}
