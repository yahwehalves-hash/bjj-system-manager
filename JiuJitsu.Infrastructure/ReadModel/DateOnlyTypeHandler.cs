using System.Data;
using Dapper;

namespace JiuJitsu.Infrastructure.ReadModel;

// Handler necessário pois Dapper não converte DateOnly <-> DateTime automaticamente
public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
        => parameter.Value = value.ToDateTime(TimeOnly.MinValue);

    public override DateOnly Parse(object value)
        => DateOnly.FromDateTime(Convert.ToDateTime(value));
}
