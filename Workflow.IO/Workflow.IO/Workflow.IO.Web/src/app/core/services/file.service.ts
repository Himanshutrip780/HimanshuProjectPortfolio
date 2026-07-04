import { Injectable, inject } from '@angular/core';
import { Observable, catchError, tap, throwError } from 'rxjs';

import { FileAttachmentResponse } from '../models/file.models';
import { getApiErrorMessage } from '../utils/api-error.util';
import { ApiHttpService } from './api-http.service';

@Injectable({ providedIn: 'root' })
export class FileService {
  private readonly api = inject(ApiHttpService);

  getTaskAttachments(taskId: string): Observable<FileAttachmentResponse[]> {
    return this.api.getList<FileAttachmentResponse>(
      `/tasks/${taskId}/attachments`,
    );
  }

  uploadAttachment(
    taskId: string,
    file: File,
  ): Observable<FileAttachmentResponse> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.api.postForm<FileAttachmentResponse>(
      `/tasks/${taskId}/attachments`,
      formData,
    );
  }

  downloadAttachment(
    fileAttachmentId: string,
    fileName: string,
  ): Observable<Blob> {
    return this.api
      .getBlob(`/attachments/${fileAttachmentId}/download`)
      .pipe(
        tap((blob) => this.triggerDownload(blob, fileName)),
        catchError((error) =>
          throwError(() => new Error(getApiErrorMessage(error, 'Download failed'))),
        ),
      );
  }

  deleteAttachment(fileAttachmentId: string): Observable<void> {
    return this.api.delete(`/attachments/${fileAttachmentId}`);
  }

  private triggerDownload(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(url);
  }
}
