using System;
using System.Reflection;
using AutoFixture.Kernel;

namespace EntityFrameworkCore.AutoFixture.Core;

public class DeclaringTypeSpecification : IRequestSpecification
{
    public DeclaringTypeSpecification(Type type)
        : this(new ExactTypeSpecification(type))
    {
    }

    public DeclaringTypeSpecification(IRequestSpecification specification)
    {
        this.Specification = specification ?? throw new ArgumentNullException(nameof(specification));
    }

    public IRequestSpecification Specification { get; }

    public bool IsSatisfiedBy(object request)
    {
        if (request is not MemberInfo memberInfo)
            return false;

        return this.Specification.IsSatisfiedBy(memberInfo.DeclaringType);
    }
}
