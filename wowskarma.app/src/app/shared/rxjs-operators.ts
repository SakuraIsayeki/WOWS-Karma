import { HttpErrorResponse, HttpResponse } from "@angular/common/http";
import { ActivatedRoute } from "@angular/router";
import {
  BehaviorSubject,
  catchError, combineLatest,
  concat,
  distinctUntilChanged,
  Observable,
  ObservableInput,
  ObservedValueOf,
  of,
  OperatorFunction,
  shareReplay, Subject,
  switchMap,
  tap,
  throwError,
} from "rxjs";
import { filter, map } from "rxjs/operators";
import { PageInfo } from 'src/app/models/PageInfo';
import { StrictHttpResponse } from "../services/api/strict-http-response";

export function reloadWhen<T>(triggerObs: Observable<any>): OperatorFunction<T, T> {
  return source$ => new Observable<T>(subscriber => {
    let lastValue: T | undefined;
    const sourceSubscription = source$.pipe(tap(v => lastValue = v)).subscribe(v => subscriber.next(v), err => subscriber.error(err), () => subscriber.complete());
    const triggerSubscription = triggerObs.subscribe(() => {
      if (lastValue !== undefined)
        subscriber.next(lastValue);
    });
    return () => {
      sourceSubscription.unsubscribe();
      triggerSubscription.unsubscribe();
    };
  });
}
/**
 * RxJS operator to catch errors and return an Observable of the error.
 * This operator is used to catch errors in the HTTP requests.
 * @returns {Observable<any>}
 * @see https://stackoverflow.com/questions/52370506/how-to-keep-observable-alive-after-error-in-rxjs-6-and-angular-6
 */
export function switchMapCatchError<T, O extends ObservableInput<any>>(project: (value: T, index: number) => O) {
  return (source$: Observable<T>) => source$.pipe(switchMap(v => {
    return of(v).pipe(switchMap(project), catchErrorReturnNull());
  }));
}

type ApiModelState<T> = {
  notFound?: true,
  model?: T
}

export function mapApiModelState<T, X>(project: (value: T, index: number) => ObservableInput<X>) {
  return (source$: Observable<T>) => source$.pipe(switchMap(v => {
    return of(v).pipe(
      switchMap(project),
      map(model => ({ model } as ApiModelState<X>)),
      catchError(err => {
        return of({ notFound: true } as ApiModelState<X>);

        // if (err instanceof HttpErrorResponse) {
        //   if (err.status == 404 || err.status == 204) {
        //     return of({ notFound: true } as ApiModelState<X>);
        //   }
        // }
        // return throwError(err);
      }),
    );
  }));
}

/**
 * RxJS operator to catch errors and return an Observable of null.
 * @returns {Observable<any>}
 */
export function catchErrorReturnNull<T>(): (source$: Observable<T>) => Observable<T | null> {
  return (source$: Observable<T>) => source$.pipe(catchError(err => {
    console.error(err);
    return of(null) as Observable<T | null>;
  }));
}

/**
 * Sets the refCount to true on the observable,
 * so that the observable will be disposed when all subscribers are unsubscribed.
 * @param bufferSize
 */
export function shareReplayRefCount<T>(bufferSize: number): (source$: Observable<T>) => Observable<T> {
  return (source$: Observable<T>) => source$.pipe(shareReplay({ refCount: true, bufferSize: bufferSize }));
}

/**
 * Checks whether the given value is not null or undefined, and returns the observable in its non-nullable type.
 */
export function filterNotNull<T>(): (source$: Observable<T>) => Observable<NonNullable<T>> {
  return (source$: Observable<T>) => source$.pipe(filter(v => v != null)) as Observable<NonNullable<T>>;
}

/**
 * Checks whether the given value is not null or undefined, and returns the observable in its non-partial type.
 */
export function filterPartials<T>(): (source$: Observable<Partial<T>>) => Observable<T> {
  return (source$: Observable<Partial<T>>) => source$.pipe(filter(v => v != null)) as Observable<T>;
}

/**
 * Executes the same action on next, error and complete events.
 * @param func
 */
export function tapAny<T>(func: () => void): (source$: Observable<T>) => Observable<T> {
  return (source$: Observable<T>) => source$.pipe(tap({
    next: () => func(),
    error: () => func(),
    complete: () => func(),
  }));
}

export function routeParam(route: ActivatedRoute, name?: string) {
  name = name ?? "id";
  return route.params.pipe(map(params => params[name!] as string | undefined), distinctUntilChanged());
}

export function routeParamInt(route: ActivatedRoute, name?: string) {
  name = name ?? "id";
  return route.params.pipe(map(params => {
    const intParam = +params[name!];
    return isNaN(intParam) ? null : intParam;
  }), distinctUntilChanged());
}

export function routeParamIntNotNull(route: ActivatedRoute, name?: string) {
  return routeParamInt(route, name).pipe(filterNotNull());
}


/**
 * Defines a decorator for an Input component property.
 */
export function InputObservable() {
  return function (target: any, key: string) {
    const subjects = new WeakMap<object, BehaviorSubject<any>>();
    const backingFields = new WeakMap<object, any>();

    Object.defineProperty(target, key, {
      configurable: false,
      get() {
        return backingFields.get(this);
      },
      set(value) {
        backingFields.set(this, value);
        subjects.get(this)?.next(value);
      },
    });

    Object.defineProperty(target, key + "$", {
      configurable: false,
      get() {
        let subject = subjects.get(this);
        if (!subject) {
          subject = new BehaviorSubject<any>(backingFields.get(this));
          subjects.set(this, subject);
        }
        return subject.asObservable();
      },
    });
  };
}

export function startFrom<T, O extends ObservableInput<any>>(start: O): OperatorFunction<T, T | ObservedValueOf<O>> {
  return (source: Observable<T>) => concat(start, source);
}

export function mergeAndCache<T, X>(merge$: Observable<X>, func: ((cached: T[], newItem: X) => T[])): (source$: Observable<T[]>) => Observable<T[]> {
  return source$ => new Observable<T[]>(subscriber => {
    let cached: T[] = [];
    const sourceSubscription = source$.subscribe(v => {
      cached = v;
      subscriber.next(v);
    }, err => subscriber.error(err), () => subscriber.complete());
    const mergeSubscription = merge$.subscribe(v => {
      cached = func(cached, v);
      subscriber.next(cached);
    });
    return () => {
      sourceSubscription.unsubscribe();
      mergeSubscription.unsubscribe();
    };
  });
}


export const anyTrue = (...observables: Array<ObservableInput<boolean>>) => combineLatest(observables).pipe(map(values => values.find(v => v == true) != undefined ), distinctUntilChanged());

/**
 * Taps the page information of a paged result, from the header of the HTTP response.
 * @param pageInfo$
 */
export const tapPageInfoHeaders = <T>(pageInfo$: BehaviorSubject<PageInfo | null>) => tap((response: StrictHttpResponse<T> | null) => {
  const pageInfo = {
    currentPage: +response?.headers.get("Content-Page-Current")! + 1,
    totalPages: +response?.headers.get("Content-Page-Total")!,
    pageSize: +response?.headers.get("Content-Page-Size")!,
    totalItems: +response?.headers.get("Content-Items-Total")!,
  }

  // Does it conform to the expected format (numbers only)? Is everything there?
  if (Object.values(pageInfo).every(v => !isNaN(v))) {
    // Then send it to the subject.
    pageInfo$.next(pageInfo);
  }

  console.debug("Page info:", pageInfo);
});
