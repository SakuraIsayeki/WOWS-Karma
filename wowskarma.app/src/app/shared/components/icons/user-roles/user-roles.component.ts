import { ChangeDetectionStrategy, Component, input, Input } from '@angular/core';
import { NgbTooltip } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'icon-user-roles',
  templateUrl: './user-roles.component.html',
  styleUrls: ['./user-roles.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    NgbTooltip
  ]
})
export class UserRolesComponent {
  userRoles = input<number[]>([]);

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
