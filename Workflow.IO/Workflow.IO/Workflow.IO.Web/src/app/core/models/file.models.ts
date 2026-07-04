export interface FileAttachmentResponse {
  fileAttachmentId: string;
  taskId: string;
  uploadedById: string;
  originalFileName: string;
  contentType: string;
  sizeInBytes: number;
  createdAt: string;
}
