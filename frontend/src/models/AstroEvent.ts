export type AstroLocation = {
  longitude: number;
  latitude: number;
  elevation: number;
}

export type AstroDates = {
  from: string;
  to: string;
}

export type AstroEntry = {
  id: string;
  name: string;
}

export type AstroCell = {
  [key: string]: any;
}

export type AstroRow = {
  entry: AstroEntry;
  cells: AstroCell[];
}

export type AstroTable = {
  header: string[];
  rows: AstroRow[];
}

export type AstroData = {
  dates: AstroDates;
  observer: {
    location: AstroLocation;
  };
  table: AstroTable;
}

export type AstroResponse = {
  data: AstroData;
}
