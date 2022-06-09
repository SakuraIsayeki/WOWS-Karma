export interface AuthModel {
  id: number,
  username: string;
  roles?: string[];

  expiration: Date;
}
