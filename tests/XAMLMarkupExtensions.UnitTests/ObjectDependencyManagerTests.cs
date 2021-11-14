namespace XAMLMarkupExtensions.UnitTests
{
    #region Uses
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Moq;
    using Xunit;
    using XAMLMarkupExtensions.Base;
    #endregion
    
    /// <summary>
    /// Tests for <see cref="ObjectDependencyManager" />.
    /// </summary>
    public class ObjectDependencyManagerTests
    {
        #region Tests of 'AddObjectDependency' method

        /// <summary>
        /// Check that it's not possible to pass null as target.
        /// </summary>
        [Fact]
        public void AddObjectDependency_TargetIsNull_ArgumentNullExceptionThrown()
        {
            // ARRANGE.
            CreateWeakReference(out var dependency, out var dependencyWeakReference);

            // ACT + ASSERT.
            var exception = Assert.Throws<ArgumentNullException>("objToHold",
                () => ObjectDependencyManager.AddObjectDependency(dependencyWeakReference, null));
            Assert.Equal("The objToHold cannot be null (Parameter 'objToHold')", exception.Message);
            
            // Prevent references optimization. 
            Assert.NotNull(dependency);
        }
        
        /// <summary>
        /// Check that it's not possible to pass null as dependency.
        /// </summary>
        [Fact]
        public void AddObjectDependency_DependencyIsNull_ArgumentNullExceptionThrown()
        {
            // ARRANGE.
            CreateWeakReference(out var target, out var targetWeakReference);

            // ACT + ASSERT.
            var exception = Assert.Throws<ArgumentNullException>("weakRefDp",
                () => ObjectDependencyManager.AddObjectDependency(null, target));
            Assert.Equal("The weakRefDp cannot be null (Parameter 'weakRefDp')", exception.Message);
            
            // Prevent references optimization. 
            Assert.NotNull(target);
        }
        
        /// <summary>
        /// Check that it's not possible to pass WeakReference as target.
        /// </summary>
        [Fact]
        public void AddObjectDependency_TargetIsWeakReference_ArgumentExceptionThrown()
        {
            // ARRANGE.
            CreateWeakReference(out var target, out var targetWeakReference);
            CreateWeakReference(out var dependency, out var dependencyWeakReference);

            // ACT + ASSERT.
            var exception = Assert.Throws<ArgumentException>("objToHold",
                () => ObjectDependencyManager.AddObjectDependency(dependencyWeakReference, targetWeakReference));
            Assert.Equal("objToHold cannot be type of WeakReference (Parameter 'objToHold')", exception.Message);
            
            // Prevent references optimization. 
            Assert.NotNull(target);
            Assert.NotNull(dependency);
        }
        
        /// <summary>
        /// Check that it's not possible to pass as target and dependency same objects.
        /// </summary>
        [Fact]
        public void AddObjectDependency_TargetAndDependencyAreSameObject_InvalidOperationExceptionThrown()
        {
            // ARRANGE.
            CreateWeakReference(out var target, out var targetWeakReference);

            // ACT + ASSERT.
            var exception = Assert.Throws<InvalidOperationException>(
                () => ObjectDependencyManager.AddObjectDependency(targetWeakReference, target));
            Assert.Equal("The WeakReference.Target cannot be the same as objToHold", exception.Message);
            
            // Prevent references optimization. 
            Assert.NotNull(target);
        }
        
        /// <summary>
        /// Check that same weak reference of dependency will not be registered twice.
        /// </summary>
        [Fact]
        public void AddObjectDependency_TwoSameDependencies_SecondIsNotRegistered()
        {
            // ARRANGE.
            CreateWeakReference(out var target, out var targetWeakReference);
            CreateWeakReference(out var dependency, out var dependencyWeakReference);
            
            // ACT.
            // Register target and twice its dependency.
            var isFirstTimeRegistered = ObjectDependencyManager.AddObjectDependency(dependencyWeakReference, target);
            var isSecondTimeRegistered = ObjectDependencyManager.AddObjectDependency(dependencyWeakReference, target);
            
            // ASSERT.
            Assert.True(isFirstTimeRegistered);
            Assert.False(isSecondTimeRegistered);
            
            // Prevent references optimization. 
            Assert.NotNull(target);
            Assert.NotNull(dependency);
        }
        
        /// <summary>
        /// Check that ObjectDependencyManager doesn't allows GC collect target, during there is exists alive dependency.
        /// </summary>
        [Fact]
        public void AddObjectDependency_DependencyAlive_TargetAlive()
        {
            // ARRANGE.
            CreateWeakReference(out var target, out var targetWeakReference);
            CreateWeakReference(out var dependency, out var dependencyWeakReference);
            
            // ACT.
            // Register target and its dependency.
            var isRegistered = ObjectDependencyManager.AddObjectDependency(dependencyWeakReference, target);
            
            // Remove direct reference to target.
            target = null;

            // Force collect. ObjectDependencyManager should has direct reference to target, so it will not collected.
            GC.Collect();

            // ASSERT.
            Assert.True(isRegistered);
            Assert.True(targetWeakReference.IsAlive);
            
            // Prevent references optimization. 
            Assert.NotNull(dependency);
        }

        /// <summary>
        /// Check that that ObjectDependencyManager doesn't allows GC collect two target, during there is exists alive dependency.
        /// </summary>
        [Fact]
        public void AddObjectDependency_TwoTargets_AllIsAlive()
        {
            // ARRANGE.
            CreateWeakReference(out var firstTarget, out var firstTargetWeakReference);
            CreateWeakReference(out var firstDependency, out var firstDependencyWeakReference);
            CreateWeakReference(out var secondTarget, out var secondTargetWeakReference);
            CreateWeakReference(out var secondDependency, out var secondDependencyWeakReference);

            // ACT.
            // Register targets and its dependencies.
            var isFirstRegistered = ObjectDependencyManager.AddObjectDependency(firstDependencyWeakReference, firstTarget);
            var isSecondRegistered = ObjectDependencyManager.AddObjectDependency(secondDependencyWeakReference, secondTarget);
            
            // Remove direct references to targets.
            firstTarget = null;
            secondTarget = null;
            
            // Force collect. ObjectDependencyManager should has direct references to targets, so they will not collected.
            GC.Collect();

            // ASSERT.
            Assert.True(isFirstRegistered);
            Assert.True(isSecondRegistered);
            Assert.True(firstTargetWeakReference.IsAlive);
            Assert.True(secondTargetWeakReference.IsAlive);
            
            // Prevent references optimization.
            Assert.NotNull(firstDependency);
            Assert.NotNull(secondDependency);
        }
        
        /// <summary>
        /// Check that AddObjectDependency method calls CleanUp inside and remove first target if its dependencies are dead.
        /// </summary>
        [Fact]
        public void AddObjectDependency_TwoTargetsFirstIsDead_FirstTargetCleaned()
        {
            // ARRANGE.
            CreateWeakReference(out var firstTarget, out var firstTargetWeakReference);
            CreateWeakReference(out var firstDependency, out var firstDependencyWeakReference);
            CreateWeakReference(out var secondTarget, out var secondTargetWeakReference);
            CreateWeakReference(out var secondDependency, out var secondDependencyWeakReference);

            // ACT.
            // Register first target and its dependency.
            var isFirstRegistered = ObjectDependencyManager.AddObjectDependency(firstDependencyWeakReference, firstTarget);
            
            // Remove direct references to first target and to the dependency.
            firstTarget = null;
            firstDependency = null;
            
            // Force collect first dependency.
            GC.Collect();
            
            // First dependency is collected.
            Assert.False(firstDependencyWeakReference.IsAlive);
            
            // First target should be still alive, because ObjectDependencyManager doesn't know that dependency is dead.
            Assert.True(firstTargetWeakReference.IsAlive);
            
            // Register second target and its dependency. After that should cleaned first target. 
            var isSecondRegistered = ObjectDependencyManager.AddObjectDependency(secondDependencyWeakReference, secondTarget);
            
            // Force collect first target.
            GC.Collect();

            // ASSERT.
            Assert.True(isFirstRegistered);
            Assert.True(isSecondRegistered);
            Assert.False(firstTargetWeakReference.IsAlive);
            
            // Prevent references optimization.
            Assert.NotNull(secondTarget);
            Assert.NotNull(secondDependency);
        }

        /// <summary>
        /// Check that AddObjectDependency method calls CleanUp inside.
        /// This case checked that <see cref="IObjectDependency.OnDependenciesRemoved" /> will be called instead <see cref="IObjectDependency.OnAllDependenciesRemoved" />,
        /// if cleaning dependencies for passed target. 
        /// </summary>
        [Fact]
        public void AddObjectDependency_OneTargetTwoDependenciesFirstIsDead_OnDependenciesRemovedIsCalled()
        {
            // ARRANGE.
            var targetMock = new Mock<IObjectDependency>();
            IReadOnlyList<WeakReference> actualWeakReferences = null;
            targetMock
                .Setup(t => t.OnDependenciesRemoved(It.IsAny<IEnumerable<WeakReference>>()))
                // TODO Save collection with ToList() here, because ObjectDependencyManager cleans passed list.
                .Callback<IEnumerable<WeakReference>>(deadDependencies => actualWeakReferences = deadDependencies.ToList());
            var target = targetMock.Object;
            
            CreateWeakReference(out var firstDependency, out var firstDependencyWeakReference);
            CreateWeakReference(out var secondDependency, out var secondDependencyWeakReference);

            // ACT.
            // Register first target and its dependency.
            var isFirstRegistered = ObjectDependencyManager.AddObjectDependency(firstDependencyWeakReference, target);
            
            // Remove direct references to target and to the first dependency.
            firstDependency = null;
            
            // Force collect first dependency.
            GC.Collect();
            
            // First dependency is collected.
            Assert.False(firstDependencyWeakReference.IsAlive);
            
            // Register second dependency.
            var isSecondRegistered = ObjectDependencyManager.AddObjectDependency(secondDependencyWeakReference, target);

            // ASSERT.
            Assert.True(isFirstRegistered);
            Assert.True(isSecondRegistered);
            Assert.True(secondDependencyWeakReference.IsAlive);
            
            targetMock.Verify(t =>
                t.OnDependenciesRemoved(It.IsAny<IEnumerable<WeakReference>>()),
                Times.Once);
            Assert.Equal(new [] { firstDependencyWeakReference }, actualWeakReferences);
            targetMock.VerifyNoOtherCalls();
            
            // Prevent references optimization.
            Assert.NotNull(secondDependency);
        }
        
        #endregion

        #region Tests of 'CleanUp' method
       
        /// <summary>
        /// Check that ObjectDependencyManager allows GC collect target, if there is no exists alive dependency.
        /// </summary>
        [Fact]
        public void CleanUp_DependencyDead_TargetDead()
        {
            // ARRANGE.
            CreateWeakReference(out var target, out var targetWeakReference);
            CreateWeakReference(out var dependency, out var dependencyWeakReference);

            // ACT.
            // Register target and its dependency.
            var isRegistered = ObjectDependencyManager.AddObjectDependency(dependencyWeakReference, target);
            
            // Remove direct references to target and dependency.
            target = null;
            dependency = null;
            
            // Force collect dependency.
            GC.Collect();
            
            // Dependency is collected.
            Assert.False(dependencyWeakReference.IsAlive);
            
            // Target should be still alive, because ObjectDependencyManager doesn't know that dependency is dead.
            Assert.True(targetWeakReference.IsAlive);
            
            // Remove the last direct reference to target.
            ObjectDependencyManager.CleanUp();
            
            // Force collect target.
            GC.Collect();

            // ASSERT.
            Assert.True(isRegistered);
            Assert.False(targetWeakReference.IsAlive);
        }
        
        /// <summary>
        /// Check that ObjectDependencyManager doesn't allows GC collect target,
        /// If one dependency is dead, but another is still alive.
        /// </summary>
        [Fact]
        public void CleanUp_TwoDependenciesOneIsDead_TargetAlive()
        {
            // ARRANGE.
            CreateWeakReference(out var target, out var targetWeakReference);
            CreateWeakReference(out var firstDependency, out var firstDependencyWeakReference);
            CreateWeakReference(out var secondDependency, out var secondDependencyWeakReference);

            // ACT.
            // Register target and its dependencies.
            var isFirstRegistered = ObjectDependencyManager.AddObjectDependency(firstDependencyWeakReference, target);
            var isSecondRegistered = ObjectDependencyManager.AddObjectDependency(secondDependencyWeakReference, target);

            // Remove direct references to target and to the first dependency.
            target = null;
            firstDependency = null;

            // Force collect dependency.
            GC.Collect();
            
            // Dependency is collected.
            Assert.False(firstDependencyWeakReference.IsAlive);
            
            // Remove information about the first dependency.
            ObjectDependencyManager.CleanUp();
            
            // Force collect target.
            GC.Collect();

            // ASSERT.
            Assert.True(isFirstRegistered);
            Assert.True(isSecondRegistered);
            Assert.True(targetWeakReference.IsAlive);
            
            // Prevent references optimization. 
            Assert.NotNull(secondDependency);
        }

        /// <summary>
        /// Check that ObjectDependencyManager remove all dead targets after clean.
        /// </summary>
        [Fact]
        public void CleanUp_TwoDependenciesDead_TwoTargetsDead()
        {
            // ARRANGE.
            CreateWeakReference(out var firstTarget, out var firstTargetWeakReference);
            CreateWeakReference(out var firstDependency, out var firstDependencyWeakReference);
            CreateWeakReference(out var secondTarget, out var secondTargetWeakReference);
            CreateWeakReference(out var secondDependency, out var secondDependencyWeakReference);
            
            // ACT.
            // Register targets and its dependencies.
            var isFirstRegistered = ObjectDependencyManager.AddObjectDependency(firstDependencyWeakReference, firstTarget);
            var isSecondRegistered = ObjectDependencyManager.AddObjectDependency(secondDependencyWeakReference, secondTarget);

            // Prevent references optimization. 
            Assert.NotNull(firstDependency);
            Assert.NotNull(secondDependency);
            
            // Remove all direct references.
            firstTarget = null;
            firstDependency = null;
            secondTarget = null;
            secondDependency = null;
            
            // Force collect dependencies.
            GC.Collect();
            
            // Dependencies are collected.
            Assert.False(firstDependencyWeakReference.IsAlive);
            Assert.False(secondDependencyWeakReference.IsAlive);
            
            // Targets should be still alive, because ObjectDependencyManager doesn't know that dependencies are dead.
            Assert.True(firstTargetWeakReference.IsAlive);
            Assert.True(secondTargetWeakReference.IsAlive);

            // Remove the last direct references to targets.
            ObjectDependencyManager.CleanUp();
            
            // Force collect targets.
            GC.Collect();

            // ASSERT.
            Assert.True(isFirstRegistered);
            Assert.True(isSecondRegistered);
            Assert.False(firstTargetWeakReference.IsAlive);
            Assert.False(secondDependencyWeakReference.IsAlive);
        }
        
        /// <summary>
        /// Check that ObjectDependencyManager calls <see cref="IObjectDependency.OnAllDependenciesRemoved" /> method if all dependencies are dead.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void CleanUp_AllDependenciesDead_CalledOnAllDependenciesRemoved(int dependenciesCount)
        {
            // ARRANGE.
            var objectDependencyMock = new Mock<IObjectDependency>();
            var objectDependency = objectDependencyMock.Object;
            var isRegistered = true;
            CreateWeakReferences(dependenciesCount, out var dependencies, out var dependencyWeakReferences);

            // ACT.
            // Register target and its dependencies.
            foreach (var dependencyWeakReference in dependencyWeakReferences)
            {
                isRegistered &= ObjectDependencyManager.AddObjectDependency(dependencyWeakReference, objectDependency);
            }

            // Remove direct references to dependencies.
            dependencies.Clear();
            dependencies = null;
            
            // Force collect dependencies.
            GC.Collect();

            // Check all dependencies are dead.
            Assert.All(dependencyWeakReferences, wr => Assert.False(wr.IsAlive));
            
            // Remove dead references.
            ObjectDependencyManager.CleanUp();

            // ASSERT.
            Assert.True(isRegistered);
            
            // Check call notify method.
            objectDependencyMock.Verify(od => od.OnAllDependenciesRemoved(), Times.Once);
            objectDependencyMock.VerifyNoOtherCalls();
        }
        
        /// <summary>
        /// Check that ObjectDependencyManager calls <see cref="IObjectDependency.OnDependenciesRemoved" /> method if some of dependencies (not all) are dead.
        /// </summary>
        [Fact]
        public void CleanUp_TwoDependenciesOneIsDead_CalledOnDependenciesRemoved()
        {
            // ARRANGE.
            var objectDependencyMock = new Mock<IObjectDependency>();
            var target = objectDependencyMock.Object;
            CreateWeakReference(out var firstDependency, out var firstDependencyWeakReference);
            CreateWeakReference(out var secondDependency, out var secondDependencyWeakReference);
            objectDependencyMock
                .Setup(od => od.OnDependenciesRemoved(It.Is<IEnumerable<WeakReference>>(
                    dp => dp.Single() == firstDependencyWeakReference)))
                .Verifiable();

            // ACT.
            // Register target and its dependencies.
            var isFirstRegistered = ObjectDependencyManager.AddObjectDependency(firstDependencyWeakReference, target);
            var isSecondRegistered = ObjectDependencyManager.AddObjectDependency(secondDependencyWeakReference, target);

            // Remove direct references to the first dependency.
            firstDependency = null;

            // Force collect dependency.
            GC.Collect();
            
            // Dependency is collected.
            Assert.False(firstDependencyWeakReference.IsAlive);
            
            // Remove information about the first dependency.
            ObjectDependencyManager.CleanUp();

            // ASSERT.
            Assert.True(isFirstRegistered);
            Assert.True(isSecondRegistered);
            Assert.True(secondDependencyWeakReference.IsAlive);
            
            // Check call notify method.
            objectDependencyMock.Verify();
            objectDependencyMock.VerifyNoOtherCalls();
            
            // Prevent references optimization. 
            Assert.NotNull(secondDependency);
        }
        
        #endregion
    
        #region Tests of 'CleanUp(target)' method

        /// <summary>
        /// Check that ObjectDependencyManager remove only one strong reference to target and GC collect it.
        /// Second strong reference should stay alive.
        /// </summary>
        [Fact]
        public void CleanUp_TwoTargetsOneCleaned_SecondAlive()
        {
            // ARRANGE.
            CreateWeakReference(out var firstTarget, out var firstTargetWeakReference);
            CreateWeakReference(out var firstDependency, out var firstDependencyWeakReference);
            CreateWeakReference(out var secondTarget, out var secondTargetWeakReference);
            CreateWeakReference(out var secondDependency, out var secondDependencyWeakReference);
            
            // ACT.
            // Register targets and its dependencies.
            var isFirstRegistered = ObjectDependencyManager.AddObjectDependency(firstDependencyWeakReference, firstTarget);
            var isSecondRegistered = ObjectDependencyManager.AddObjectDependency(secondDependencyWeakReference, secondTarget);

            // Remove direct references to second target and its dependency.
            secondTarget = null;
            secondDependency = null;
            
            // Remove first target from ObjectDependencyManager.
            ObjectDependencyManager.CleanUp(firstTarget);
            
            // Remove direct reference to first target.
            firstTarget = null;

            // Force collect. ObjectDependencyManager should has direct reference to first target, so it will collected.
            // But its should still reference to second target.
            GC.Collect();

            // ASSERT.
            Assert.True(isFirstRegistered);
            Assert.True(isSecondRegistered);
            Assert.False(firstTargetWeakReference.IsAlive);
            Assert.True(secondTargetWeakReference.IsAlive);
            Assert.False(secondDependencyWeakReference.IsAlive);
            
            // Prevent references optimization.
            Assert.NotNull(firstDependency);
        }
        
        /// <summary>
        /// Check that ObjectDependencyManager remove all dead targets after clean if pass `null` as target.
        /// </summary>
        [Fact]
        public void CleanUp_TargetIsNullAndTwoDependenciesDead_TwoTargetsDead()
        {
            // ARRANGE.
            CreateWeakReference(out var firstTarget, out var firstTargetWeakReference);
            CreateWeakReference(out var firstDependency, out var firstDependencyWeakReference);
            CreateWeakReference(out var secondTarget, out var secondTargetWeakReference);
            CreateWeakReference(out var secondDependency, out var secondDependencyWeakReference);
            
            // ACT.
            // Register targets and its dependencies.
            var isFirstRegistered = ObjectDependencyManager.AddObjectDependency(firstDependencyWeakReference, firstTarget);
            var isSecondRegistered = ObjectDependencyManager.AddObjectDependency(secondDependencyWeakReference, secondTarget);

            // Prevent references optimization. 
            Assert.NotNull(firstDependency);
            Assert.NotNull(secondDependency);
            
            // Remove all direct references.
            firstTarget = null;
            firstDependency = null;
            secondTarget = null;
            secondDependency = null;
            
            // Force collect dependencies.
            GC.Collect();
            
            // Dependencies are collected.
            Assert.False(firstDependencyWeakReference.IsAlive);
            Assert.False(secondDependencyWeakReference.IsAlive);
            
            // Targets should be still alive, because ObjectDependencyManager doesn't know that dependencies are dead.
            Assert.True(firstTargetWeakReference.IsAlive);
            Assert.True(secondTargetWeakReference.IsAlive);

            // Remove the last direct references to targets.
            ObjectDependencyManager.CleanUp(null);
            
            // Force collect targets.
            GC.Collect();

            // ASSERT.
            Assert.True(isFirstRegistered);
            Assert.True(isSecondRegistered);
            Assert.False(firstTargetWeakReference.IsAlive);
            Assert.False(secondDependencyWeakReference.IsAlive);
        }
        
        #endregion
        
        #region Helper methods

        /// <summary>
        /// Create object and weak reference which hold this object.
        /// </summary>
        /// <param name="obj">The created object.</param>
        /// <param name="weakReference">The weak reference of created object.</param>
        /// <remarks>
        /// This method allows fix problem with GC.
        /// If object created and cleared inside one method, GC often don't collect its.
        /// But if object created in one method and cleared in another (in test method), then this not problem to collect its.
        /// </remarks>
        private static void CreateWeakReference(out object obj, out WeakReference weakReference)
        {
            obj = new object();
            weakReference = new WeakReference(obj);
        }

        /// <summary>
        /// Create list of objects and weak references which hold this objects.
        /// </summary>
        /// <param name="count">The count of objects.</param>
        /// <param name="objects">The list of created objects.</param>
        /// <param name="weakReferences">The list of weak references of created objects.</param>
        /// <remarks>
        /// This method allows fix problem with GC.
        /// If list created and cleared inside one method, GC often don't collect items inside its.
        /// But if list created in one method and cleared in another (in test method), then this not problem to collect its items.
        /// </remarks>
        private static void CreateWeakReferences(int count, out List<object> objects, out List<WeakReference> weakReferences)
        {
            objects = new List<object>(count);
            weakReferences = new List<WeakReference>(count);
            
            for (int i = 0; i < count; ++i)
            {
                CreateWeakReference(out var obj, out var weakReference);
                objects.Add(obj);
                weakReferences.Add(weakReference);
            }
        }
        
        #endregion
    }
}