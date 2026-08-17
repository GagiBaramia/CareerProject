import { Component, computed, input } from '@angular/core';
import { API_BASE_URL } from '../../../core/config/api-config';

@Component({
  selector: 'app-avatar',
  standalone: true,
  template: `
    @if (fullImageUrl()) {
      <img
        class="avatar"
        [style.width.px]="size()"
        [style.height.px]="size()"
        [src]="fullImageUrl()"
        [alt]="name()"
      />
    } @else {
      <div
        class="avatar avatar-fallback"
        [style.width.px]="size()"
        [style.height.px]="size()"
        [style.fontSize.px]="size() / 2.2"
      >
        {{ initial() }}
      </div>
    }
  `,
  styleUrl: './avatar.component.css'
})
export class AvatarComponent {
  imageUrl = input<string | null>(null);
  name = input('');
  size = input(40);

  // Uploaded images are served by the backend behind the Gateway (relative /uploads/... paths),
  // not by the Angular dev server - the full URL has to go through the Gateway origin.
  fullImageUrl = computed(() => (this.imageUrl() ? `${API_BASE_URL}${this.imageUrl()}` : null));
  initial = computed(() => this.name().trim().charAt(0).toUpperCase() || '?');
}
