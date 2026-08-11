type Location = {
  id: string;
  name: string;
  address: Address;
  timezone: string;
  createdAt: Date;
  updatedAt: Date;
};

type Address = {
  city: string;
  district: string;
  street: string;
  structure: string;
};
