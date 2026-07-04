export interface CreateCommentRequest {
  body: string;
  parentCommentId?: string | null;
  mentionedUserIds?: string[];
}

export interface UpdateCommentRequest {
  body: string;
  mentionedUserIds?: string[];
}

export interface CommentResponse {
  commentId: string;
  taskId: string;
  authorId: string;
  parentCommentId: string | null;
  body: string;
  mentionedUserIds: string[];
  isDeleted: boolean;
  createdAt: string;
  updatedAt: string;
}
