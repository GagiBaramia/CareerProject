import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SkillsService } from '../../../../core/services/skills.service';
import { PROFICIENCY_LEVELS, ProficiencyLevel, ProfileSkill, Skill } from '../../../../core/models/profile.models';

@Component({
  selector: 'app-skills-autocomplete',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './skills-autocomplete.component.html',
  styleUrl: './skills-autocomplete.component.css'
})
export class SkillsAutocompleteComponent {
  private readonly skillsService = inject(SkillsService);

  @Input() selected: ProfileSkill[] = [];
  @Output() selectedChange = new EventEmitter<ProfileSkill[]>();

  readonly levels = PROFICIENCY_LEVELS;
  readonly query = signal('');
  readonly suggestions = signal<Skill[]>([]);
  readonly isOpen = signal(false);

  private readonly querySubject = new Subject<string>();

  constructor() {
    this.querySubject
      .pipe(
        debounceTime(200),
        distinctUntilChanged(),
        switchMap((q) => this.skillsService.search(q)),
        takeUntilDestroyed()
      )
      .subscribe((skills) => {
        const selectedIds = new Set(this.selected.map((s) => s.skillId));
        this.suggestions.set(skills.filter((s) => !selectedIds.has(s.id)));
      });
  }

  onQueryChange(value: string): void {
    this.query.set(value);
    this.isOpen.set(true);
    this.querySubject.next(value);
  }

  onFocus(): void {
    this.isOpen.set(true);
    this.querySubject.next(this.query());
  }

  onBlur(): void {
    // Delay so a click on a suggestion registers before the list closes.
    setTimeout(() => this.isOpen.set(false), 150);
  }

  addSkill(skill: Skill): void {
    const next: ProfileSkill[] = [
      ...this.selected,
      { skillId: skill.id, skillName: skill.name, level: 'Intermediate' as ProficiencyLevel }
    ];
    this.selectedChange.emit(next);
    this.query.set('');
    this.suggestions.set([]);
    this.isOpen.set(false);
  }

  removeSkill(skillId: string): void {
    this.selectedChange.emit(this.selected.filter((s) => s.skillId !== skillId));
  }

  updateLevel(skillId: string, level: string): void {
    const next = this.selected.map((s) =>
      s.skillId === skillId ? { ...s, level: level as ProficiencyLevel } : s
    );
    this.selectedChange.emit(next);
  }
}
