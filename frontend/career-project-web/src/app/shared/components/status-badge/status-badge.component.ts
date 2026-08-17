import { Component, computed, input } from '@angular/core';
import { applicationStatusLabel } from '../../../core/models/application.models';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  template: `<span class="status-badge" [class]="'status-' + status().toLowerCase()">{{ label() }}</span>`,
  styleUrl: './status-badge.component.css'
})
export class StatusBadgeComponent {
  status = input.required<string>();
  label = computed(() => applicationStatusLabel(this.status()));
}
