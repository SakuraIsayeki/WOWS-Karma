import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'app-unauthorized',
    templateUrl: './unauthorized.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [RouterLink]
})
export class UnauthorizedComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
