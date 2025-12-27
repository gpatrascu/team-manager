export interface Team {
  id: string;
  name: string;
  description?: string;
  admins: string[];
  inviteToken?: string;
  inviteTokenExpiry?: Date;
  members: TeamMember[];
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface TeamMember {
  id: string;
  userId?: string;
  name: string;
  email: string;
  nickname: string;
  role: string;
  status: 'pending' | 'active' | 'inactive';
  joinedAt: Date;
  invitedBy?: string;
  approvedBy?: string;
  approvedAt?: Date;
}

// Request DTOs
export interface CreateTeamRequest {
  name: string;
  description?: string;
}

export interface GenerateInviteTokenRequest {
  expiryHours?: number;
}

export interface GenerateInviteTokenResult {
  inviteToken: string;
  expiryDate: Date;
}

export interface JoinTeamRequest {
  inviteToken: string;
  nickname: string;
}
