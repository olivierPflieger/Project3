describe('tests', () => {
   
  it('Navigate to home page should work', () => {
    
    cy.visit('/')
    
    // assert
    cy.get('[data-cy="home-text"]').should('contain.text', 'Tu veux partager un fichier ?')    

  })
})