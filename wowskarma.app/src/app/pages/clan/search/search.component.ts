import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from "@angular/forms";
import { BehaviorSubject, debounceTime, distinctUntilChanged, tap } from "rxjs";
import { map } from "rxjs/operators";
import { ClanService } from "../../../services/api/services/clan.service";
import { filterNotNull, switchMapCatchError, tapAny } from "../../../shared/rxjs-operators";
import { NgIf, NgFor, AsyncPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ColorHexPipe } from '../../../services/pipes/colorHex.pipe';

@Component({
    templateUrl: "./search.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [ReactiveFormsModule, NgIf, NgFor, RouterLink, AsyncPipe, ColorHexPipe]
})
export class SearchComponent {
  search = new FormGroup({
    clanName: new FormControl(""),
  });

  loading$ = new BehaviorSubject<boolean>(false);

  // Gets an Observable to detect when the search form value changes.
  result$ = this.search.valueChanges.pipe(
      map(s => s.clanName), // Select the username field
      debounceTime(300),
      distinctUntilChanged(),
      filterNotNull(),
      tap(() => this.loading$.next(true)), // Set loading to true when the search form value changes.
      switchMapCatchError(search => this.clanService.apiClanSearchSearchGet$Json({ search })), // Get the search results
      tapAny(() => this.loading$.next(false)) // Sets loading to false when the search results are received, regardless of whether the search was successful or not.
  );


  constructor(
      public clanService: ClanService,
  ) { }

  /*
  onSubmit() {
    this.playerService.apiPlayerSearchQueryGet$Json({ query: this.search.value.username as string })
      .subscribe(value => this.results = value);
  }
  */
}
