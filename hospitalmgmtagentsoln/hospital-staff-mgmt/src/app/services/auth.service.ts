import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = 'http://localhost:5095/api/Authorization/login';

  constructor(private http: HttpClient) { }

  login(data: { username: string; password: string }): Observable<{ loginResponse: string, threadId: string }> {
    var response =  this.http.post<{ loginResponse: string, threadId: string }>(this.apiUrl, data);
    
    return response;
  }

  logout(): Observable<any> {
    const threadId = localStorage.getItem('threadId');
    const token = localStorage.getItem('token');

    if (!threadId || !token) {
      console.warn('Missing threadId or token');
      return of(null); // prevent error if called without data
    }

    return this.http.post(`http://localhost:5095/api/Authorization/logout?threadId=${encodeURIComponent(threadId)}`, null, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });
  }
}
