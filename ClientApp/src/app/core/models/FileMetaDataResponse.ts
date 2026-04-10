export interface FileMetaDataResponse {
  originalFileName: string,
  fileSize: string,
  extension: string,
  token: string,
  createdAt: string,
  isExpired: boolean,
  expirationDays: number,
  expirationDate: string,
  tags: string
}