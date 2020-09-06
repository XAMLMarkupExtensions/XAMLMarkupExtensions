namespace XAMLMarkupExtensions.UnitTests
{
    #region Uses
    using System;
    using XAMLMarkupExtensions.Base;
    using Xunit;
    #endregion
    
    /// <summary>
    /// Tests for <see cref="ObjectDependencyManager" />.
    /// </summary>
    public class ObjectDependencyManagerTests
    {
        /// <summary>
        /// Check that it's not possible to pass null as target.
        /// </summary>
        [Fact]
        public void AddObjectDependency_TargetIsNull_ArgumentNullExceptionThrown()
        {
            CreateWeakReference(out var dependency, out var dependencyWeakReference);

            
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
            CreateWeakReference(out var target, out var targetWeakReference);

            
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
            CreateWeakReference(out var target, out var targetWeakReference);
            CreateWeakReference(out var dependency, out var dependencyWeakReference);

            
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
            CreateWeakReference(out var target, out var targetWeakReference);

            
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
            CreateWeakReference(out var target, out var targetWeakReference);
            CreateWeakReference(out var dependency, out var dependencyWeakReference);

            
            // Register target and twice its dependency.
            var isFirstTimeRegistered = ObjectDependencyManager.AddObjectDependency(dependencyWeakReference, target);
            var isSecondTimeRegistered = ObjectDependencyManager.AddObjectDependency(dependencyWeakReference, target);
            
            
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
            CreateWeakReference(out var target, out var targetWeakReference);
            CreateWeakReference(out var dependency, out var dependencyWeakReference);
            
            
            // Register target and its dependency.
            var isRegistered = ObjectDependencyManager.AddObjectDependency(dependencyWeakReference, target);
            
            // Remove direct reference to target.
            target = null;

            // Force collect. ObjectDependencyManager should has direct reference to target, so it will not collected.
            GC.Collect();


            Assert.True(isRegistered);
            Assert.True(targetWeakReference.IsAlive);
            
            // Prevent references optimization. 
            Assert.NotNull(dependency);
        }
        
        /// <summary>
        /// Check that ObjectDependencyManager allows GC collect target, if there is no exists alive dependency.
        /// </summary>
        [Fact]
        public void AddObjectDependency_DependencyDead_TargetDead()
        {
            CreateWeakReference(out var target, out var targetWeakReference);
            CreateWeakReference(out var dependency, out var dependencyWeakReference);

            
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


            Assert.True(isRegistered);
            Assert.False(targetWeakReference.IsAlive);
        }

        /// <summary>
        /// Check that ObjectDependencyManager doesn't allows GC collect target,
        /// If one dependency is dead, but another is still alive.
        /// </summary>
        [Fact]
        public void AddObjectDependency_TwoDependenciesOneIsDead_TargetAlive()
        {
            CreateWeakReference(out var target, out var targetWeakReference);
            CreateWeakReference(out var firstDependency, out var firstDependencyWeakReference);
            CreateWeakReference(out var secondDependency, out var secondDependencyWeakReference);


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


            Assert.True(isFirstRegistered);
            Assert.True(isSecondRegistered);
            Assert.True(targetWeakReference.IsAlive);
            
            // Prevent references optimization. 
            Assert.NotNull(secondDependency);
        }
        
        /// <summary>
        /// Check that that ObjectDependencyManager doesn't allows GC collect two target, during there is exists alive dependency.
        /// </summary>
        [Fact]
        public void AddObjectDependency_TwoTargets_AllIsAlive()
        {
            CreateWeakReference(out var firstTarget, out var firstTargetWeakReference);
            CreateWeakReference(out var firstDependency, out var firstDependencyWeakReference);
            CreateWeakReference(out var secondTarget, out var secondTargetWeakReference);
            CreateWeakReference(out var secondDependency, out var secondDependencyWeakReference);


            // Register targets and its dependencies.
            var isFirstRegistered = ObjectDependencyManager.AddObjectDependency(firstDependencyWeakReference, firstTarget);
            var isSecondRegistered = ObjectDependencyManager.AddObjectDependency(secondDependencyWeakReference, secondTarget);
            
            // Remove direct references to targets.
            firstTarget = null;
            secondTarget = null;
            
            // Force collect. ObjectDependencyManager should has direct references to targets, so they will not collected.
            GC.Collect();


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
            CreateWeakReference(out var firstTarget, out var firstTargetWeakReference);
            CreateWeakReference(out var firstDependency, out var firstDependencyWeakReference);
            CreateWeakReference(out var secondTarget, out var secondTargetWeakReference);
            CreateWeakReference(out var secondDependency, out var secondDependencyWeakReference);


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


            Assert.True(isFirstRegistered);
            Assert.True(isSecondRegistered);
            Assert.False(firstTargetWeakReference.IsAlive);
            
            // Prevent references optimization.
            Assert.NotNull(secondTarget);
            Assert.NotNull(secondDependency);
        }
        
        
        /// <summary>
        /// Check that ObjectDependencyManager remove strong reference to target and GC collect it.
        /// </summary>
        [Fact]
        public void CleanUp_TargetNotRegistered_ExceptionThrown()
        {
            CreateWeakReference(out var target, out _);
            

            var exception = Assert.Throws<Exception>(() => ObjectDependencyManager.CleanUp(target));
            Assert.Equal("Key was not found!", exception.Message);
            
            // Prevent references optimization.
            Assert.NotNull(target);
        }
        
        /// <summary>
        /// Check that ObjectDependencyManager remove only one strong reference to target and GC collect it.
        /// Second strong reference should stay alive.
        /// </summary>
        [Fact]
        public void CleanUp_TwoTargetsOneCleaned_SecondAlive()
        {
            CreateWeakReference(out var firstTarget, out var firstTargetWeakReference);
            CreateWeakReference(out var firstDependency, out var firstDependencyWeakReference);
            CreateWeakReference(out var secondTarget, out var secondTargetWeakReference);
            CreateWeakReference(out var secondDependency, out var secondDependencyWeakReference);
            
            
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


            Assert.True(isFirstRegistered);
            Assert.True(isSecondRegistered);
            Assert.False(firstTargetWeakReference.IsAlive);
            Assert.True(secondTargetWeakReference.IsAlive);
            Assert.False(secondDependencyWeakReference.IsAlive);
            
            // Prevent references optimization.
            Assert.NotNull(firstDependency);
        }
        
        /// <summary>
        /// Check that ObjectDependencyManager remove all dead targets after clean.
        /// </summary>
        [Fact]
        public void CleanUp_TwoDependenciesDead_TwoTargetsDead()
        {
            CreateWeakReference(out var firstTarget, out var firstTargetWeakReference);
            CreateWeakReference(out var firstDependency, out var firstDependencyWeakReference);
            CreateWeakReference(out var secondTarget, out var secondTargetWeakReference);
            CreateWeakReference(out var secondDependency, out var secondDependencyWeakReference);
            
            
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


            Assert.True(isFirstRegistered);
            Assert.True(isSecondRegistered);
            Assert.False(firstTargetWeakReference.IsAlive);
            Assert.False(secondDependencyWeakReference.IsAlive);
        }
        
        
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
    }
}