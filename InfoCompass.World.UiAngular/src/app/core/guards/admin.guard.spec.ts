import { Router } from '@angular/router';
import { AuthenticationService } from 'src/app/core/services/auth.service';
import { AdminGuard } from './admin.guard';

describe('AdminGuard', () => {

    // let router: jasmine.SpyObj<Router>;
    // let authService: jasmine.SpyObj<AuthenticationService>;

    // beforeEach(() => {
    //     router = jasmine.createSpyObj<Router>('Router', ['navigate']);
    //     authService = jasmine.createSpyObj<AuthenticationService>('AuthenticationService', ['getCurrentlyLoggedInUser']);
    // });

    // it('create an instance', () => {
    //     const guard = new AdminGuard(router, authService);
    //     expect(guard).toBeTruthy();
    // });

    // it('returns true if user is admin', () => {
    //     const user = { 'isAdministrator': true };
    //     authService.getCurrentlyLoggedInUser.and.returnValue(user);
    //     const guard = new AdminGuard(router, authService);

    //     const result = guard.canActivate();

    //     expect(result).toBe(true);
    // });

    // it('returns false if user does not exist', () => {
    //     authService.getCurrentlyLoggedInUser.and.returnValue(null);
    //     const guard = new AdminGuard(router, authService);

    //     const result = guard.canActivate();

    //     expect(result).toBe(false);
    // });

    // it('returns false if user is not admin', () => {
    //     const user = { 'isAdministrator': false };
    //     authService.getCurrentlyLoggedInUser.and.returnValue(user);
    //     const guard = new AdminGuard(router, authService);

    //     const result = guard.canActivate();

    //     expect(result).toBe(false);
    // });

    // it('redirects to root if user is not an admin', () => {
    //     const user = { 'isAdministrator': false };
    //     authService.getCurrentlyLoggedInUser.and.returnValue(user);
    //     const guard = new AdminGuard(router, authService);

    //     guard.canActivate();

    //     expect(router.navigate).toHaveBeenCalledWith(['/']);
    // });

    // it('redirects to root if user does not exist', () => {
    //     authService.getCurrentlyLoggedInUser.and.returnValue(null);
    //     const guard = new AdminGuard(router, authService);

    //     guard.canActivate();

    //     expect(router.navigate).toHaveBeenCalledWith(['/']);
    // });

});
