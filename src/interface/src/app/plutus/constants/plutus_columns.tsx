import { abbreviateNumber } from "@/app/components/pretty_number";
import { GridColDef } from "@mui/x-data-grid";

export const plutusColumns: GridColDef[] = [
    {
        field: "symbolName",
        headerName: "Name",
        flex: 1
    },
    {
        field: "symbolSubcode",
        headerName: "Subcode",
        flex: 1
    },
    {
        field: "minPrice",
        headerName: "Min Price",
        valueFormatter: x => abbreviateNumber(x),
        type: "number",
        flex: 1
    },
    {
        field: "maxPrice",
        headerName: "Max Price",
        valueFormatter: x => abbreviateNumber(x),
        type: "number",
        flex: 1
    },
    {
        field: "averagePrice",
        headerName: "Average Price",
        valueFormatter: x => abbreviateNumber(x),
        type: "number",
        flex: 1
    },
    {
        field: "totalVolume",
        headerName: "Volume",
        valueFormatter: x => abbreviateNumber(x),
        type: "number",
        flex: 1
    },
    {
        field: "limit",
        headerName: "Limit",
        valueFormatter: x => abbreviateNumber(x),
        type: "number",
        flex: 1
    },
    {
        field: "margin",
        headerName: "Margin",
        valueFormatter: x => abbreviateNumber(x),
        type: "number",
        flex: 1
    },
    {
        field: "totalGain",
        headerName: "Gain",
        valueFormatter: x => abbreviateNumber(x),
        type: "number",
        flex: 1
    },
    {
        field: "roi",
        headerName: "ROI",
        valueGetter: x => `${Math.round(x * 100)}%`,
        type: "number",
        flex: 1
    }
]