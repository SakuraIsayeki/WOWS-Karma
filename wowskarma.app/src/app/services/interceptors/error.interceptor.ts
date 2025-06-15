import { HttpRequest, HttpEvent, HttpHandlerFn } from '@angular/common/http';
import { catchError, Observable } from "rxjs";

export function errorInterceptor(request: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> {
  return next(request).pipe(catchError(err => {
    // do whatever with the error
    throw err;
  }));
}
