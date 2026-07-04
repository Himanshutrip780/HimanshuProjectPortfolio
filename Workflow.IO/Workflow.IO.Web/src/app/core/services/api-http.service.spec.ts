import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { ApiHttpService } from './api-http.service';

describe('ApiHttpService', () => {
  let service: ApiHttpService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ApiHttpService],
    });

    service = TestBed.inject(ApiHttpService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('get unwraps ApiResponse payload', () => {
    service.get<{ name: string }>('/projects').subscribe((data) => {
      expect(data.name).toBe('Demo');
    });

    const req = httpMock.expectOne((r) => r.url.endsWith('/projects'));
    expect(req.request.method).toBe('GET');
    req.flush({
      success: true,
      message: 'ok',
      data: { name: 'Demo' },
    });
  });

  it('getList returns empty array on 204', () => {
    service.getList<{ id: string }>('/users').subscribe((data) => {
      expect(data).toEqual([]);
    });

    const req = httpMock.expectOne((r) => r.url.endsWith('/users'));
    req.flush(null, { status: 204, statusText: 'No Content' });
  });
});
