"use client";

import { DataGrid, GridColDef, GridModel } from "@/app/components/core/data-display/data_grid";
import Typography from "@/app/components/core/data-display/typography";
import RefreshIcon from "@/app/components/core/icons/refresh_icon";
import Button from "@/app/components/core/inputs/button";
import IconButton from "@/app/components/core/inputs/icon_button";
import Box from "@/app/components/core/layout/box";
import { hasPaginationChanged, mapFilter, mapOrder, mapPagination } from "@/app/components/core/utils/graphql_mappers";
import { abbreviateNumber } from "@/app/components/utils/pretty_number";
import PaginationInfo from "@/app/models/pagination_info";
import { GET_MARKET_FORECAST } from "@/app/plutus/queries";
import { useQuery } from "@urql/next";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";

interface RowData {
    id: string;
    symbolId: string;
    symbolName: string;
    symbolSubcode: string | null | undefined;
    "latest.averagePrice": number;
    "dayOne.margin": number;
    "dayOne.gain": number;
    "dayTwo.averagePrice": number;
    "dayTwo.margin": number;
    "dayTwo.gain": number;
}

export default function RecentMarketTrades() {
    const router = useRouter();
    const { marketId } = useParams<{ marketId: string }>();
    const [paginationInfo, setPaginationInfo] = useState<PaginationInfo>();
    const [gridModel, setGridModel] = useState<GridModel>({
        sortModel: [{ field: "dayOne.gain", sort: "desc" }],
        paginationModel: { page: 0, pageSize: 10 },
        filterModel: { items: [] }
    });

    const handleRowClick = (row: RowData) => {
        router.push(`/plutus/explorer/${marketId}/${row.symbolId}?referrer=forecasts`);
    };

    const handleBackClicked = () => {
        router.push("/plutus/forecasts");
    };

    const handleGridModelChanged = (model: GridModel) => {
        if (hasPaginationChanged(model.paginationModel, gridModel.paginationModel)) {
            const paginationInfo = mapPagination(model.paginationModel, gridModel.paginationModel, data?.marketForecast?.pageInfo);
            setPaginationInfo(paginationInfo);
        }

        setGridModel(model);
    };

    const columns: GridColDef[] = [
        {
            field: "symbolName",
            headerName: "Name",
            flex: 1
        },
        {
            field: "symbolSubcode",
            headerName: "Subcode",
            flex: 1,
        },
        {
            field: "latest.averagePrice",
            headerName: "Yesterday's Price",
            flex: 1,
            type: "number",
            valueFormatter: x => abbreviateNumber(x),
        },
        {
            field: "dayOne.averagePrice",
            headerName: "Today's Price",
            flex: 1,
            type: "number",
            valueFormatter: x => abbreviateNumber(x),
        },
        {
            field: "dayOne.margin",
            headerName: "Today's Margin",
            flex: 1,
            type: "number",
            valueFormatter: x => abbreviateNumber(x),
        },
        {
            field: "dayOne.gain",
            headerName: "Today's Gain",
            flex: 1,
            type: "number",
            valueFormatter: x => abbreviateNumber(x),
        },
        {
            field: "dayTwo.averagePrice",
            headerName: "Tomorrow's Price",
            flex: 1,
            type: "number",
            valueFormatter: x => abbreviateNumber(x),
        },
        {
            field: "dayTwo.margin",
            headerName: "Tomorrow's Margin",
            flex: 1,
            type: "number",
            valueFormatter: x => abbreviateNumber(x),
        },
        {
            field: "dayTwo.gain",
            headerName: "Tomorrow's Gain",
            flex: 1,
            type: "number",
            valueFormatter: x => abbreviateNumber(x),
        }
    ]

    const [{ data, fetching }, reexecute] = useQuery({
        query: GET_MARKET_FORECAST,
        variables: {
            marketId: marketId,
            where: mapFilter(gridModel.filterModel, columns),
            order: mapOrder(gridModel.sortModel),
            after: paginationInfo?.after,
            first: paginationInfo?.first,
            before: paginationInfo?.before,
            last: paginationInfo?.last
        }
    });

    const transformedData: RowData[] = data?.marketForecast?.nodes?.map(x => {
        return {
            id: x.id,
            symbolId: x.symbolId,
            symbolName: x.symbolName,
            symbolSubcode: x.symbolSubcode,
            "latest.averagePrice": x.latest.averagePrice,
            "dayOne.averagePrice": x.dayOne.averagePrice,
            "dayOne.margin": x.dayOne.margin,
            "dayOne.gain": x.dayOne.gain,
            "dayTwo.averagePrice": x.dayTwo.averagePrice,
            "dayTwo.margin": x.dayTwo.margin,
            "dayTwo.gain": x.dayTwo.gain
        };
    }) || [];

    const timeToRefresh = (): string => {
        const now = new Date();
        const midnightUTC = new Date(Date.UTC(
            now.getUTCFullYear(),
            now.getUTCMonth(),
            now.getUTCDate() + 1,
            0, 0, 0, 0
        ));

        const timeDifferenceMs = midnightUTC.getTime() - now.getTime();
        const totalSeconds = Math.floor(timeDifferenceMs / 1000);
        const hours = Math.floor(totalSeconds / 3600);
        const minutes = Math.floor((totalSeconds % 3600) / 60);
        const seconds = totalSeconds % 60;

        if (hours == 0 && minutes == 0) {
            return `${seconds} seconds`;
        } else if (minutes == 0) {
            return `${minutes} minutes`;
        } else {
            return `${hours}  hours`;
        }
    }

    return (
        <>
            <Box
                styling={{
                    width: "100%",
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    m: "auto"
                }}
            >
                <Box>
                    <Button variant="outlined" onClick={handleBackClicked}>
                        Back
                    </Button>
                </Box>

                <Box
                    styling={{
                        display: 'flex',
                        alignItems: 'center',
                        gap: "small"
                    }}
                >
                    <Typography variant="body1">
                        Next forecasts generated in {timeToRefresh()}
                    </Typography>
                    <IconButton disabled={fetching} onClick={reexecute}>
                        <RefreshIcon />
                    </IconButton>
                </Box>
            </Box>

            <DataGrid
                rows={transformedData}
                columns={columns}
                getRowId={(row: RowData) => row.id}
                rowCount={data?.marketForecast?.totalCount || 0}
                onRowClick={handleRowClick}
                loading={fetching}
                initialModel={gridModel}
                onGridModelChange={handleGridModelChanged}
                pageSizeOptions={[5, 10, 15, 20, 50]}
                styling={{ mt: 'medium' }}
                toolbar
            />
        </>
    );
}