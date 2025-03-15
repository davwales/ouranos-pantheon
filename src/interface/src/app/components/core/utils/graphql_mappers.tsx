import { getFieldType, GridColDef } from "@/app/components/core/data-display/data_grid";
import PaginationInfo from "@/app/models/pagination_info";
import { PageInfo, SortEnumType } from "@/gql/graphql";
import { GridFilterModel, GridLogicOperator, GridPaginationModel, GridSortModel } from "@mui/x-data-grid";

const createNestedFilterObject = (fieldPath: string, value: any, operator: string) => {
    const segments = fieldPath.split('.');
    let currentObject = { [operator]: value };

    for (let i = segments.length - 1; i >= 0; i--) {
        currentObject = { [segments[i]]: currentObject };
    }

    return currentObject;
}

export function mapFilter(
    model: GridFilterModel | undefined,
    columns: GridColDef[]
): any {
    if (!model) {
        return undefined;
    }

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
                    return null;
                }
            }

            return createNestedFilterObject(field, parsedValue, mappedOperator);
        })
        .filter(Boolean);

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

const createNestedOrderObject = (fieldPath: string, sortDirection: SortEnumType) => {
    const segments = fieldPath.split('.');
    let currentObject: any = sortDirection;

    for (let i = segments.length - 1; i >= 0; i--) {
        currentObject = { [segments[i]]: currentObject };
    }

    return currentObject;
}

export function mapOrder(sortModel: GridSortModel | undefined): any {
    if (!sortModel) {
        return undefined;
    }

    return sortModel.map(({ field, sort }) => {
        return createNestedOrderObject(field, sort?.toUpperCase() as SortEnumType);
    });
};

export function mapPagination(
    paginationModel?: GridPaginationModel,
    previousPaginationModel?: GridPaginationModel,
    pageInfo?: PageInfo
): PaginationInfo {
    if (!paginationModel) {
        return {};
    }

    if (!previousPaginationModel) {
        return {
            first: paginationModel.pageSize
        }
    }

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
    paginationModel?: GridPaginationModel,
    previousPaginationModel?: GridPaginationModel
): boolean {
    return paginationModel?.page !== previousPaginationModel?.page ||
        paginationModel?.pageSize !== previousPaginationModel?.pageSize;
}
