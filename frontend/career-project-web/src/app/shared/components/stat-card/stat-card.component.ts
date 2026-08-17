import { Component, input } from '@angular/core';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  template: `
    <div class="stat-card">
      <div class="stat-icon">{{ icon() }}</div>
      <div class="stat-body">
        <div class="stat-value">{{ value() }}</div>
        <div class="stat-label">{{ label() }}</div>
      </div>
    </div>
  `,
  styleUrl: './stat-card.component.css'
})
export class StatCardComponent {
  icon = input('📊');
  value = input.required<string | number>();
  label = input.required<string>();
}
