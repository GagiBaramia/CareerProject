import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { SessionService } from '../services/session.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthService).getToken();
  const session = inject(SessionService);

  const authedReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authedReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Only an *authenticated* request coming back 401 means the session itself is invalid -
      // a 401 on an unauthenticated request (e.g. wrong password on /api/auth/login) is a normal
      // outcome the calling component already handles, not a reason to force a logout.
      if (token && error.status === 401) {
        session.logout();
      }

      return throwError(() => error);
    })
  );
};
