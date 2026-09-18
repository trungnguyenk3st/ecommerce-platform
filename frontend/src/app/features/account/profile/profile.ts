import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './profile.html'
})
export class ProfileComponent {
  private readonly fb = inject(FormBuilder);
  readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);

  readonly profileForm = this.fb.nonNullable.group({
    fullName: [this.auth.currentUser()?.fullName ?? '', Validators.required]
  });

  readonly passwordForm = this.fb.nonNullable.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8)]]
  });

  readonly savingProfile = signal(false);
  readonly savingPassword = signal(false);
  readonly passwordError = signal<string | null>(null);

  saveProfile(): void {
    if (this.profileForm.invalid) return;
    this.savingProfile.set(true);
    this.auth.updateProfile(this.profileForm.getRawValue().fullName).subscribe({
      next: () => { this.savingProfile.set(false); this.toast.success('Profile updated.'); },
      error: () => this.savingProfile.set(false)
    });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) return;
    this.savingPassword.set(true);
    this.passwordError.set(null);
    const { currentPassword, newPassword } = this.passwordForm.getRawValue();

    this.auth.changePassword(currentPassword, newPassword).subscribe({
      next: () => {
        this.savingPassword.set(false);
        this.passwordForm.reset();
        this.toast.success('Password changed.');
      },
      error: err => {
        this.savingPassword.set(false);
        this.passwordError.set(err?.error?.errors ? Object.values(err.error.errors).flat().join(' ') : 'Could not change password.');
      }
    });
  }
}
