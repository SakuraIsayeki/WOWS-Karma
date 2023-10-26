import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
  selector: 'icon-user-roles',
  templateUrl: './user-roles.component.html',
  styleUrls: ['./user-roles.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserRolesComponent {
  @Input() UserRoles: number[] = [];

  constructor() { }

  protected readonly UserRolesMap = UserRolesMap;
}

const UserRolesMap = [
  {
    "id": 1,
    "name": "Platform Administrator",
    "icon": "bi-tools",
    "color": "#e74c3c"
  },
  {
    "id": 2,
    "name": "Community Manager",
    "icon": "bi-shield-shaded",
    "color": "#1abc9c"
  }
]
