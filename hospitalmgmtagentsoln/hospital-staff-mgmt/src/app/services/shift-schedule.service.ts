import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ChatResponse } from '../models/chat-response.model';
import { ShiftScheduleResponse } from '../models/shift-schedule-response.model';

@Injectable({ providedIn: 'root' })
export class ShiftScheduleService {
  constructor(private http: HttpClient) {}

  fetchShiftInformation(): Observable<ShiftScheduleResponse[]> {
     const body = { null: null }; // Matches the C# model
     return this.http.post<ShiftScheduleResponse[]>('http://localhost:5095/Schedule/fetch', body );

  }
}