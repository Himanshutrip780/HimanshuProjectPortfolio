import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  CommentResponse,
  CreateCommentRequest,
  UpdateCommentRequest,
} from '../models/comment.models';
import { ApiHttpService } from './api-http.service';

@Injectable({ providedIn: 'root' })
export class CommentService {
  private readonly api = inject(ApiHttpService);

  getTaskComments(taskId: string): Observable<CommentResponse[]> {
    return this.api.get<CommentResponse[]>(`/tasks/${taskId}/comments`);
  }

  createComment(
    taskId: string,
    request: CreateCommentRequest,
  ): Observable<CommentResponse> {
    return this.api.post<CommentResponse>(
      `/tasks/${taskId}/comments`,
      request,
    );
  }

  updateComment(
    commentId: string,
    request: UpdateCommentRequest,
  ): Observable<CommentResponse> {
    return this.api.put<CommentResponse>(`/comments/${commentId}`, request);
  }

  deleteComment(commentId: string): Observable<void> {
    return this.api.delete(`/comments/${commentId}`);
  }
}
