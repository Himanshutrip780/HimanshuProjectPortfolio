import { ApiResponse } from '../models/api-response.model';
import {
  toHttpParams,
  unwrapApiResponse,
  unwrapApiResponseOrEmpty,
} from './api.util';

describe('api.util', () => {
  it('unwrapApiResponse returns data when successful', () => {
    const response: ApiResponse<{ id: string }> = {
      success: true,
      message: 'ok',
      data: { id: '1' },
    };

    expect(unwrapApiResponse(response)).toEqual({ id: '1' });
  });

  it('unwrapApiResponse throws when unsuccessful', () => {
    const response: ApiResponse<null> = {
      success: false,
      message: 'failed',
      data: null,
    };

    expect(() => unwrapApiResponse(response)).toThrowError('failed');
  });

  it('unwrapApiResponseOrEmpty returns empty array on 204', () => {
    expect(unwrapApiResponseOrEmpty<string[]>(null, 204)).toEqual([]);
  });

  it('unwrapApiResponseOrEmpty unwraps when body present', () => {
    const response: ApiResponse<string[]> = {
      success: true,
      message: 'ok',
      data: ['a'],
    };

    expect(unwrapApiResponseOrEmpty(response)).toEqual(['a']);
  });

  it('toHttpParams skips empty values', () => {
    const params = toHttpParams({ q: 'demo', status: null, page: 2 });
    expect(params.get('q')).toBe('demo');
    expect(params.get('page')).toBe('2');
    expect(params.has('status')).toBeFalse();
  });
});
