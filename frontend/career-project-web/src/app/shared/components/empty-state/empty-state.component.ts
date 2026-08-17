import { Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  template: `
    <div class="empty-state">
      <div class="empty-icon">{{ icon() }}</div>
      <p class="empty-message">{{ message() }}</p>
      <ng-content></ng-content>
    </div>
  `,
  styleUrl: './empty-state.component.css'
})
export class EmptyStateComponent {
  icon = input('📭');
  message = input.required<string>();
}
