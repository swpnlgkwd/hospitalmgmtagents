import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.html',
  standalone: true,
  imports: [ReactiveFormsModule]
})
export class Login  {
  loginForm: FormGroup;
  loading = false;

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
    });
  }

  onLogin(): void {
    if (this.loginForm.invalid) return;

    this.loading = true;
    const { username, password } = this.loginForm.value;

    this.auth.login({ username, password }).subscribe({
      next: (res:any) => {
        console.log('Login successful:', res);
        console.log(res);
        this.loading = false;        
        this.router.navigate(['/calendar']); // Or appropriate role-based route
        localStorage.setItem('token', res.loginResponse.token);
        localStorage.setItem('threadId', res.threadId);
        console.log(res.threadId);
      },
      error: () => {
        this.loading = false;
        alert('Invalid username or password');
      },
    });
  }
}
