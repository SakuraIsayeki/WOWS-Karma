import { Injectable } from '@angular/core';
import { BehaviorSubject, startWith } from "rxjs";

@Injectable({providedIn: "root"})
export class AppInitService {

  private isInitialized = new BehaviorSubject<boolean>(false);
  public isInitialized$ = this.isInitialized.pipe(
    //delay(this.isBrowser() ? 500 : 0),
    startWith(false));

  constructor() {
  }

  public initialized() {
    this.isInitialized.next(true);
  }
}
