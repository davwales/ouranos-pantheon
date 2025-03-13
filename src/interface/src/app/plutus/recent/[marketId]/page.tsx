"use client";

import { DataGrid, GridColDef, GridModel } from "@/app/components/core/data-display/data_grid";
import Button from "@/app/components/core/inputs/button";
import Box from "@/app/components/core/layout/box";
import { abbreviateNumber } from "@/app/components/utils/pretty_number";
import useInterval from "@/app/components/utils/use_interval";
import { GET_RECENT_MARKET_TRADES } from "@/app/plutus/queries";
import { useQuery } from "@urql/next";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";

interface RecentTradeRow {
    id: number;
    symbolId: string;
    symbolName: string;
    symbolSubcode: string | null | undefined;
    price: number;
    volume: number;
    createdAt: Date;
}

export default function RecentMarketTrades() {
    const router = useRouter();
    const { marketId } = useParams<{ marketId: string }>();
    const [gridModel, setGridModel] = useState<GridModel>({
        sortModel: [{ field: "totalGain", sort: "desc" }],
        paginationModel: { page: 0, pageSize: 10 },
        filterModel: { items: [] }
    });

    const handleBackClicked = () => {
        router.push("/plutus/recent");
    };

    const handleRowClick = (row: RecentTradeRow) => {
        router.push(`/plutus/explorer/${marketId}/${row.symbolId}?referrer=recent`);
    };

    const handleGridModelChanged = (model: GridModel) => {
        setGridModel({
            ...gridModel,
            paginationModel: {
                page: 0,
                pageSize: model.paginationModel.pageSize
            }
        });
    };

    const [{ data, fetching }, reexecute] = useQuery({
        query: GET_RECENT_MARKET_TRADES,
        variables: {
            marketId: marketId,
            first: gridModel.paginationModel.pageSize,
        }
    });

    const transformedData: RecentTradeRow[] = data?.allTrades?.nodes?.map((x, i) => {
        return {
            id: i,
            symbolId: x.metadata.symbolId,
            symbolName: x.metadata.symbolName,
            symbolSubcode: x.metadata.symbolSubcode,
            price: x.price,
            volume: x.volume,
            createdAt: x.createdAt
        }
    }) || [];

    const columns: GridColDef[] = [
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
            field: "price",
            headerName: "Price",
            valueFormatter: x => abbreviateNumber(x),
            type: "number",
            flex: 1
        },
        {
            field: "volume",
            headerName: "Volume",
            valueFormatter: x => abbreviateNumber(x),
            type: "number",
            flex: 1
        },
        {
            field: "createdAt",
            headerName: "Date",
            valueGetter: x => new Date(x).toLocaleString(),
            type: "string",
            flex: 1
        }
    ]

    useInterval(() => reexecute(), 15000);

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
            </Box>

            <DataGrid
                rows={transformedData}
                columns={columns}
                getRowId={(row: any) => row.id}
                rowCount={gridModel.paginationModel.pageSize}
                loading={fetching}
                initialModel={gridModel}
                onGridModelChange={handleGridModelChanged}
                pageSizeOptions={[5, 10, 15, 20, 50]}
                onRowClick={handleRowClick}
                styling={{ mt: 'medium' }}
            />
        </>
    );
}