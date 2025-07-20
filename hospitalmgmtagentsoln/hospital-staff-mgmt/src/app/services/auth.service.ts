import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = 'http://localhost:5095/api/Authorization/login';

  constructor(private http: HttpClient) {}

  login(data: { username: string; password: string }): Observable<{ token: string }> {
    return this.http.post<{ token: string }>(this.apiUrl, data);
  }
}
