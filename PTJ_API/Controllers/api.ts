export interface CreatePurchaseProposalDetailDto {
  quantity: number;
  unitPrice: number;
  manufacturer: string;
  seats: number;
  acquisitionMethod: 'Ownership' | 'Lease';
  registrationTax?: number | null;
  roadMaintenanceFee?: number | null;
  licensePlateFee?: number | null;
  insuranceFee?: number | null;
  hasCamera158: boolean;
  hasGsht: boolean;
  enginePower?: number | null;
  payloadCapacity?: number | null;
  notes?: string | null;
}

export interface CreatePurchaseProposalDto {
  description: string;
  details: CreatePurchaseProposalDetailDto[];
}

export interface VehicleReceptionDto {
  purchaseProposalId: number;
  branchId: number;
  licensePlate: string;
  vin: string;
  telematicsImei: string;
  chassisNumber: string;
  engineNumber: string;
  badgeType?: string | null;
  registrationExpirationDate: string | Date;
  insuranceExpirationDate: string | Date;
  badgeExpirationDate?: string | Date | null;
  fuelNorm?: number | null;
  receiptImageUrl: string;
  notes?: string | null;
}