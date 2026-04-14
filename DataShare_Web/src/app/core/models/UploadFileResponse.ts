export interface UploadFileResponse {
  isSuccess: boolean,
  Message: string,
  originalFileName: string,
  fileSize: string,
  token: string,
  extension: string,
  createdAt: string,
  expirationDays: number,
  tags: string
}