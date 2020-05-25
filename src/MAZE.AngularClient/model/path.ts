/**
 * MAZE
 * MAZE API
 *
 * The version of the OpenAPI document: 1.0.0
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */


export interface Path { 
    readonly id?: number;
    readonly from?: number;
    readonly to?: number;
    type?: Path.TypeEnum;
}
export namespace Path {
    export type TypeEnum = 'West' | 'East' | 'North' | 'South' | 'Portal';
    export const TypeEnum = {
        West: 'West' as TypeEnum,
        East: 'East' as TypeEnum,
        North: 'North' as TypeEnum,
        South: 'South' as TypeEnum,
        Portal: 'Portal' as TypeEnum
    };
}


