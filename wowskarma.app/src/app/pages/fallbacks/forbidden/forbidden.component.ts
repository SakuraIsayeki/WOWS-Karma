import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-forbidden',
    templateUrl: './forbidden.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true
})
export class ForbiddenComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
