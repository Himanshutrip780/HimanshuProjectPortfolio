import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { UserService } from './user.service';
import { ApiHttpService } from './api-http.service';

describe('UserService', () => {
  let service: UserService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [UserService, ApiHttpService],
    });

    service = TestBed.inject(UserService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('getMe calls /users/me', () => {
    service.getMe().subscribe((user) => {
      expect(user.email).toBe('a@test.com');
      expect(user.firstName).toBe('A');
    });

    const req = httpMock.expectOne((r) => r.url.endsWith('/users/me'));
    req.flush({
      success: true,
      message: 'ok',
      data: {
        userId: '11111111-1111-1111-1111-111111111111',
        email: 'a@test.com',
        firstName: 'A',
        lastName: 'B',
        avatarUrl: null,
        role: 'User',
        status: 0,
        isDeleted: false,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-01T00:00:00Z',
      },
    });
  });

  it('getAllUsers uses getList and returns [] on 204', () => {
    service.getAllUsers().subscribe((users) => {
      expect(users).toEqual([]);
    });

    const req = httpMock.expectOne((r) => r.url.endsWith('/users'));
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('lookupUsers calls /users/lookup with email query', () => {
    service.lookupUsers('demo').subscribe((users) => {
      expect(users.length).toBe(1);
      expect(users[0].email).toContain('demo');
    });

    const req = httpMock.expectOne(
      (r) => r.url.includes('/users/lookup') && r.params.get('email') === 'demo',
    );
    req.flush({
      success: true,
      message: 'ok',
      data: [
        {
          userId: '11111111-1111-1111-1111-111111111111',
          email: 'owner.demo@workflow.io.local',
          firstName: 'Zen',
          lastName: 'Owner',
        },
      ],
    });
  });
});
