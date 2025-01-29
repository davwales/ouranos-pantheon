import { PageInfo, SortEnumType } from "@/gql/graphql";
import { GridColDef, GridFilterModel, GridLogicOperator, GridPaginationModel, GridSortModel } from "@mui/x-data-grid";
import OuranosPaginationInfo from "../models/ouranos_pagination_info";
import { getFieldType } from "./material_helpers";

export function mapFilter(
    model: GridFilterModel,
    columns: GridColDef[]
): any {
    const filterItems = model.items
        .filter((item) => item.value !== undefined && item.value !== null)
        .map((item) => {
            const { field, operator, value } = item;
            const fieldType = getFieldType(columns, field);
            const mappedOperator = mapOperator(operator);

            let parsedValue: any = value;

            if (fieldType === "number") {
                parsedValue = parseFloat(value as string);
                if (isNaN(parsedValue)) {
                    return null; // Skip invalid numbers
                }
            }

            return {
                [field]: {
                    [mappedOperator]: parsedValue,
                },
            };
        })
        .filter(Boolean); // Remove nulls

    if (filterItems.length === 0) return null;

    if (filterItems.length === 1) {
        return filterItems[0];
    } else if (model.logicOperator === GridLogicOperator.Or) {
        return { or: filterItems };
    } else {
        return { and: filterItems };
    }
}

export function mapOperator(operator: string): string {
    switch (operator) {
        case ">":
            return "gt";
        case ">=":
            return "gte";
        case "<":
            return "lt";
        case "<=":
            return "lte";
        case "equals":
            return "eq";
        case "!=":
            return "neq";
        case "contains":
            return "contains";
        case "startsWith":
            return "startsWith";
        case "endsWith":
            return "endsWith";
        default:
            throw new Error(`Unsupported operator: ${operator}`);
    }
}

export function mapOrder(sortModel: GridSortModel): any {
    return sortModel.map(({ field, sort }) => ({
        [field]: sort?.toUpperCase() as SortEnumType,
    }))
};

export function mapPagination(
    paginationModel: GridPaginationModel,
    previousPaginationModel: GridPaginationModel,
    pageInfo?: PageInfo
): OuranosPaginationInfo {
    // Page size has changed, return the first page again
    if (paginationModel.pageSize != previousPaginationModel.pageSize) {
        return {
            first: paginationModel.pageSize
        };
    }

    if (paginationModel.page > previousPaginationModel.page) {
        // The next page has been requested.
        return {
            after: pageInfo?.endCursor || undefined,
            first: paginationModel.pageSize
        };
    } else {
        // The previous page has been requested.
        return {
            before: pageInfo?.startCursor || undefined,
            last: paginationModel.pageSize
        }
    }
}

export function hasPaginationChanged(
    paginationModel: GridPaginationModel,
    previousPaginationModel: GridPaginationModel
): boolean {
    return paginationModel.page !== previousPaginationModel.page ||
        paginationModel.pageSize !== previousPaginationModel.pageSize;
}
